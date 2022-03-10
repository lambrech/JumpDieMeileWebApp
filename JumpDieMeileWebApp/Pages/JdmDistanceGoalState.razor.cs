namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    public partial class JdmDistanceGoalState : IAsyncDisposable
    {
        private static readonly string MapContainerId = "jdm_live_distance_goal_map";

        private IJSObjectReference? mapInteropHelper;

        private IJSObjectReference? interopMap;

        private readonly SemaphoreSlim semaphore = new(1, 1);

        public JdmDistanceGoalState()
        {
            this.MileStones.Add(new JdmMileStone("Buju (Ettlingen)", "", "", "", 11.5m, (decimal)48.937939, (decimal)8.393441));
            this.MileStones.Add(new JdmMileStone("MAXX-Camp", "", "", "", 198m, (decimal)47.928595, (decimal)9.618834));
            this.MileStones.Add(new JdmMileStone("Gabrovo", "", "", "", 2111m, (decimal)42.869873, (decimal)25.314847));
            this.MileStones.Add(new JdmMileStone("Christival Erfurt", "", "", "", 4184m, (decimal)50.977181, (decimal)11.023781));
            this.MileStones.Add(new JdmMileStone("Mitarbeiterwochenende (Wildberg)", "", "", "", 4578.2m, (decimal)48.624101, (decimal)8.748582));
            this.MileStones.Add(new JdmMileStone("Spanien (Valencia)", "", "", "", 6123.4m, (decimal)39.4752036, (decimal)-0.3755116));
            this.MileStones.Add(new JdmMileStone("CVJM Jubiläum (Europapark)", "", "", "", 7714.4m, (decimal)48.268324, (decimal)7.722204));
            this.MileStones.Add(new JdmMileStone("Altenkirchen", "", "", "", 8042.4m, (decimal)50.687365, (decimal)7.645876));
            this.MileStones.Add(new JdmMileStone("Kirche Kunterbunt", "", "", "", 8304.6m, (decimal)49.2465193, (decimal)8.6882689));
            this.MileStones.Add(new JdmMileStone("Unteröwisheim", "", "", "", 8373.7m, (decimal)49.142526, (decimal)8.672482));
            this.MileStones.Add(new JdmMileStone("Kodersdorf", "", "", "", 8997.6m, (decimal)51.229676, (decimal)14.893017));
            this.MileStones.Add(new JdmMileStone("JUMP WG", "", "", "", 9591.6m, (decimal)49.01083, (decimal)8.37252));
        }

        [Parameter]
        public decimal TotalDistance { get; set; }

        [Inject]
        private IJSRuntime? JsRuntime { get; set; }

        private bool ShowSingleRuns { get; set; } = false;

        private List<JdmMileStone> MileStones { get; } = new();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (this.mapInteropHelper != null)
            {
                await this.mapInteropHelper.DisposeAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && (this.JsRuntime != null))
            {
                this.mapInteropHelper = await this.JsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./map-interop.js");

                this.interopMap = await this.mapInteropHelper.InvokeAsync<IJSObjectReference>("initMap", MapContainerId).AsTask();

                var wg = this.MileStones.Last();
                await this.mapInteropHelper.InvokeVoidAsync(
                               "addMarker",
                               this.interopMap,
                               wg.Latitude,
                               wg.Longitude,
                               $"Ziel: {wg.Location}")
                          .AsTask();

                for (var i = 0; i < this.MileStones.Count - 1; i++)
                {
                    var tmp = this.MileStones[i];
                    await this.mapInteropHelper.InvokeVoidAsync(
                                   "drawLine",
                                   this.interopMap,
                                   "#757679",
                                   0.5,
                                   wg.Latitude,
                                   wg.Longitude,
                                   tmp.Latitude,
                                   tmp.Longitude,
                                   1,
                                   false)
                              .AsTask();
                    await this.mapInteropHelper.InvokeVoidAsync(
                                   "addMarker",
                                   this.interopMap,
                                   tmp.Latitude,
                                   tmp.Longitude,
                                   $"Ziel: {tmp.Location}{(!string.IsNullOrWhiteSpace(tmp.ShortName) ? $", Heimat von {tmp.ShortName}" : "")}")
                              .AsTask();
                }

                await this.DrawAllLines();
            }
        }

        private static CalcResult CalcPercent(JdmMileStone m, JdmMileStone prevM, decimal dist)
        {
            var mileStoneDist = m.Distance - prevM.Distance;
            var currentDist = dist - prevM.Distance;

            if (dist >= m.Distance)
            {
                return new CalcResult(1, mileStoneDist, mileStoneDist, m);
            }

            if (dist <= prevM.Distance)
            {
                return new CalcResult(0, mileStoneDist, 0, m);
            }

            return new CalcResult(currentDist / mileStoneDist, mileStoneDist, currentDist, m);
        }

        private List<CalcResult> CalculateCurrentMilestoneResults()
        {
            // this surely should be called somewhere else ... but fow whatever
            this.DrawAllLines();

            var list = new List<CalcResult>();

            var prev = new JdmMileStone("", "", "", "", 0, 0, 0);
            var localDist = this.TotalDistance;
            foreach (var jdmMileStone in this.MileStones)
            {
                list.Add(CalcPercent(jdmMileStone, prev, localDist));
                prev = jdmMileStone;
            }

            return list;
        }

        private async Task DrawAllLines()
        {
            if (this.mapInteropHelper == null)
            {
                return;
            }

            var semophoreTaken = false;
            try
            {
                semophoreTaken = this.semaphore.Wait(0);

                if (semophoreTaken)
                {
                    // remove old lines
                    await this.mapInteropHelper.InvokeVoidAsync("removeLines", this.interopMap);

                    // add new lines
                    var wg = this.MileStones.Last();
                    var helper = new List<(decimal Start, decimal End, JdmMileStone jms, bool hin)>();
                    decimal tmpDist = 0;
                    foreach (var jdmMileStone in this.MileStones)
                    {
                        if (jdmMileStone == this.MileStones[^1])
                        {
                            break;
                        }

                        var dist = jdmMileStone.Distance - tmpDist;
                        helper.Add((tmpDist, jdmMileStone.Distance, jdmMileStone, false));
                        helper.Add((jdmMileStone.Distance, jdmMileStone.Distance + dist, jdmMileStone, true));
                        tmpDist = jdmMileStone.Distance + dist;
                    }

                    var localDist = this.TotalDistance;

                    foreach ((var Start, var End, JdmMileStone jms, var wayBack) in helper)
                    {
                        decimal CalcPercent(decimal start, decimal end, decimal dist)
                        {
                            if (localDist >= end)
                            {
                                return 1;
                            }

                            if (localDist <= start)
                            {
                                return 0;
                            }

                            var mileStoneDist = end - start;
                            var currentDist = dist - start;

                            return currentDist / mileStoneDist;
                        }

                        var calcResult = CalcPercent(Start, End, localDist);
                        if (calcResult > 0)
                        {
                            if (wayBack)
                            {
                                //var latFullDist = (wg.Latitude - jms.Latitude);
                                //var lngFullDist = (wg.Longitude - jms.Longitude);

                                //var latDist = latFullDist * calcResult;
                                //var lngDist = lngFullDist * calcResult;

                                await this.mapInteropHelper.InvokeVoidAsync(
                                               "drawLine",
                                               this.interopMap,
                                               "#C30039",
                                               1,
                                               jms.Latitude,
                                               jms.Longitude,
                                               wg.Latitude,
                                               wg.Longitude,
                                               calcResult)
                                          .AsTask();

                                //if (calcResult < 1)
                                //{
                                //    Console.WriteLine($"perc: {calcResult}, start: {jms.Latitude}/{jms.Longitude}, end: {wg.Latitude}/{wg.Longitude}, calc: {jms.Latitude + latDist}/{jms.Longitude + lngDist}");
                                //}
                            }
                            else
                            {
                                //var latFullDist = (jms.Latitude - wg.Latitude);
                                //var lngFullDist = (jms.Longitude - wg.Longitude);

                                //var latDist = latFullDist * calcResult;
                                //var lngDist = lngFullDist * calcResult;

                                await this.mapInteropHelper.InvokeVoidAsync(
                                               "drawLine",
                                               this.interopMap,
                                               "#C30039",
                                               0.5,
                                               wg.Latitude,
                                               wg.Longitude,
                                               jms.Latitude,
                                               jms.Longitude,
                                               calcResult)
                                          .AsTask();

                                //if (calcResult < 1)
                                //{
                                //    Console.WriteLine($"perc: {calcResult}, start: {wg.Latitude}/{wg.Longitude}, end: {jms.Latitude}/{jms.Longitude}, calc: {wg.Latitude + latDist}/{wg.Longitude + lngDist}");
                                //}
                            }
                        }
                    }
                }
            }
            finally
            {
                if (semophoreTaken)
                {
                    this.semaphore.Release();
                }
            }
        }

        public record JdmMileStone(string Location, string Plz, string FullName, string ShortName, decimal Distance, decimal Latitude, decimal Longitude);

        private record CalcResult(decimal Percent, decimal MileStoneDistance, decimal CurrentDistance, JdmMileStone MileStone);
    }
}