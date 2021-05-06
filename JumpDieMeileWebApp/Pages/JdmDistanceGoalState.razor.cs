namespace JumpDieMeileWebApp.Pages
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Components;

    public partial class JdmDistanceGoalState
    {
        [Parameter]
        public decimal TotalDistance { get; set; }

        public record JdmMileStone(string Location, string Plz, string FullName, string ShortName, decimal Distance);
        private record CalcResult(decimal Percent, decimal MileStoneDistance, decimal CurrentDistance, JdmMileStone MileStone);

        private bool ShowSingleRuns { get; set; } = true;


        private List<JdmMileStone> MileStones { get; } = new ();

        public JdmDistanceGoalState()
        {
            this.MileStones.Add(new JdmMileStone("Bühlenhausen", "89180", "Theo Karl Burkhardt", "Theo", 145));
            this.MileStones.Add(new JdmMileStone("Görlitz", "02826", "Paul Eric Hoffmann", "Paul", 945));
            this.MileStones.Add(new JdmMileStone("Graben-Neudorf", "76676", "Fiona Schneider", "Fiona", 1626));
            this.MileStones.Add(new JdmMileStone("Kodersdorf", "02923", "Ian Haupt", "Ian", 2298));
            this.MileStones.Add(new JdmMileStone("Korntal", "70825", "Niklas Liesenfeld", "Niki", 3020));
            this.MileStones.Add(new JdmMileStone("Mulsum", "27639", "Samuel Breitenmoser", "Sam", 3757));
            this.MileStones.Add(new JdmMileStone("Mutschelbach", "76307", "Lisa Henning", "Lisa", 4438));
            this.MileStones.Add(new JdmMileStone("Schemmerhausen", "51580", "Luca Brecht", "Luca", 4755));
            this.MileStones.Add(new JdmMileStone("Solingen", "42659", "Joschua Erbe", "Joshua", 5381));
            this.MileStones.Add(new JdmMileStone("Walddorfhäslach", "72141", "Marilena Müller", "Leni", 5809));
            this.MileStones.Add(new JdmMileStone("Warendorf", "48231", "Simon Eggersmann", "Simon", 6333));
            this.MileStones.Add(new JdmMileStone("Wiesa", "02923", "Luise Liebscher", "Luise", 7399));
            this.MileStones.Add(new JdmMileStone("Jerusalem", "", "", "", 12020));
            this.MileStones.Add(new JdmMileStone("JUMP WG", "", "", "", 16000));
        }

        private List<CalcResult> CalculateCurrentMilestoneResults()
        {
            var list = new List<CalcResult>();

            var prev = new JdmMileStone("", "", "", "", 0);
            var localDist = this.TotalDistance;
            foreach (var jdmMileStone in this.MileStones)
            {
                list.Add(CalcPercent(jdmMileStone, prev, localDist));
                prev = jdmMileStone;
            }

            return list;
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
                return new CalcResult(0, mileStoneDist, (decimal)0, m);
            }

            return new CalcResult(currentDist / mileStoneDist, mileStoneDist, currentDist, m);
        }
    }
}
