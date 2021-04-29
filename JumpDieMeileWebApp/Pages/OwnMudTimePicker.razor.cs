namespace JumpDieMeileWebApp.Pages
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    using MudBlazor;
    using MudBlazor.Extensions;
    using MudBlazor.Utilities;
    using static System.String;

    public partial class OwnMudTimePicker : MudPicker<TimeSpan?>, IAsyncDisposable
    {
        private readonly SetTime _timeSet = new();

        private OpenTo _currentView;

        private int _initialHour;

        private IJSObjectReference? getElementByPointsHelper;

        public OwnMudTimePicker() : base(new DefaultConverter<TimeSpan?>())
        {
            this.Converter.GetFunc = this.OnGet;
            this.Converter.SetFunc = this.OnSet;
        }

        /// <summary>
        ///     First view to show in the MudDatePicker.
        /// </summary>
        [Parameter]
        public OpenTo OpenTo { get; set; } = OpenTo.Hours;

        /// <summary>
        ///     Choose the edition mode. By default you can edit hours and minutes.
        /// </summary>
        [Parameter]
        public TimeEditMode TimeEditMode { get; set; } = TimeEditMode.Normal;

        /// <summary>
        ///     If true, sets 12 hour selection clock.
        /// </summary>
        [Parameter]
        public bool AmPm { get; set; }

        /// <summary>
        ///     Fired when the date changes.
        /// </summary>
        [Parameter]
        public EventCallback<TimeSpan?> TimeChanged { get; set; }

        public bool MouseDown { get; set; }

        /// <summary>
        ///     The currently selected time (two-way bindable). If null, then nothing was selected.
        /// </summary>
        [Parameter]
        public TimeSpan? Time
        {
            get => this._value;
            set => this.SetTimeAsync(value, true).AndForget();
        }

        internal TimeSpan? TimeIntermediate { get; private set; }

        protected string ToolbarClass =>
            new CssBuilder("mud-picker-timepicker-toolbar")
               .AddClass("mud-picker-timepicker-toolbar-landscape", (this.Orientation == Orientation.Landscape) && (this.PickerVariant == PickerVariant.Static))
               .AddClass(this.Class)
               .Build();

        protected string HoursButtonClass =>
            new CssBuilder("mud-timepicker-button")
               .AddClass("mud-timepicker-toolbar-text", this._currentView == OpenTo.Minutes)
               .Build();

        protected string MinuteButtonClass =>
            new CssBuilder("mud-timepicker-button")
               .AddClass("mud-timepicker-toolbar-text", this._currentView == OpenTo.Hours)
               .Build();

        protected string AmButtonClass =>
            new CssBuilder("mud-timepicker-button")
               .AddClass("mud-timepicker-toolbar-text", !this.IsAm) // gray it out
               .Build();

        protected string PmButtonClass =>
            new CssBuilder("mud-timepicker-button")
               .AddClass("mud-timepicker-toolbar-text", !this.IsPm) // gray it out
               .Build();

        [Inject]
        private IJSRuntime? JsRuntime { get; set; }

        private string HourDialClass =>
            new CssBuilder("mud-time-picker-hour")
               .AddClass("mud-time-picker-dial")
               .AddClass("mud-time-picker-dial-out", this._currentView != OpenTo.Hours)
               .AddClass("mud-time-picker-dial-hidden", this._currentView != OpenTo.Hours)
               .Build();

        private string MinuteDialClass =>
            new CssBuilder("mud-time-picker-minute")
               .AddClass("mud-time-picker-dial")
               .AddClass("mud-time-picker-dial-out", this._currentView != OpenTo.Minutes)
               .AddClass("mud-time-picker-dial-hidden", this._currentView != OpenTo.Minutes)
               .Build();

        private bool IsAm => (this._timeSet.Hour >= 00) && (this._timeSet.Hour < 12); // am is 00:00 to 11:59 

        private bool IsPm => (this._timeSet.Hour >= 12) && (this._timeSet.Hour < 24); // pm is 12:00 to 23:59 

        public override void Clear(bool close = true)
        {
            this.Time = null;
            this.TimeIntermediate = null;
            base.Clear();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (this.getElementByPointsHelper != null)
            {
                await this.getElementByPointsHelper.DisposeAsync();
            }
        }

        protected async Task SetTimeAsync(TimeSpan? time, bool updateValue)
        {
            if (this._value != time)
            {
                this.TimeIntermediate = time;
                this._value = time;
                if (updateValue)
                {
                    await this.SetTextAsync(this.Converter.Set(this._value), false);
                }

                this.UpdateTimeSetFromTime();
                await this.TimeChanged.InvokeAsync(this._value);
                this.BeginValidate();
            }
        }

        protected override Task StringValueChanged(string value)
        {
            this.Touched = true;
            // Update the time property (without updating back the Value property)
            return this.SetTimeAsync(this.Converter.Get(value), false);
        }

        protected override void OnPickerOpened()
        {
            base.OnPickerOpened();
            this._currentView = this.TimeEditMode switch
            {
                TimeEditMode.Normal => this.OpenTo,
                TimeEditMode.OnlyHours => OpenTo.Hours,
                TimeEditMode.OnlyMinutes => OpenTo.Minutes,
                _ => this._currentView
            };
        }

        protected override void Submit()
        {
            this.Time = this.TimeIntermediate;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.UpdateTimeSetFromTime();
            this._currentView = this.OpenTo;
            this._initialHour = this._timeSet.Hour;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && (this.JsRuntime != null))
            {
                this.getElementByPointsHelper = await this.JsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./interop.js");
            }
        }

        private string? OnSet(TimeSpan? time)
        {
            return this.AmPm ? time.ToAmPmString() : time.ToIsoString();
        }

        private TimeSpan? OnGet(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var pm = false;
            var value1 = value.Trim();
            var m = Regex.Match(value, "AM|PM", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                this.AmPm = true; // <-- this is kind of a hack, but we need to make sure it is set or else the string value might be converted to 24h format.
                pm = m.Value.ToLower() == "pm";
                value1 = Regex.Replace(value, "(AM|am|PM|pm)", "").Trim();
            }

            if (TimeSpan.TryParse(value1, out var time))
            {
                if (pm)
                {
                    time = new TimeSpan((time.Hours + 12) % 24, time.Minutes, 0);
                }

                return time;
            }

            return null;
        }

        private string GetHourString()
        {
            if (this.TimeIntermediate == null)
            {
                return "--";
            }

            var h = this.AmPm ? this.TimeIntermediate.Value.ToAmPmHour() : this.TimeIntermediate.Value.Hours;
            return Math.Min(23, Math.Max(0, h)).ToString(CultureInfo.InvariantCulture);
        }

        private string GetMinuteString()
        {
            if (this.TimeIntermediate == null)
            {
                return "--";
            }

            return $"{Math.Min(59, Math.Max(0, this.TimeIntermediate.Value.Minutes)):D2}";
        }

        private void UpdateTime()
        {
            this.TimeIntermediate = new TimeSpan(this._timeSet.Hour, this._timeSet.Minute, 0);
        }

        private void OnHourClick()
        {
            this._currentView = OpenTo.Hours;
        }

        private void OnMinutesClick()
        {
            this._currentView = OpenTo.Minutes;
        }

        private void OnAmClicked()
        {
            this._timeSet.Hour %= 12; // "12:-- am" is "00:--" in 24h
            this.UpdateTime();
        }

        private void OnPmClicked()
        {
            if (this._timeSet.Hour <= 12) // "12:-- pm" is "12:--" in 24h
            {
                this._timeSet.Hour += 12;
            }

            this._timeSet.Hour %= 24;
            this.UpdateTime();
        }

        private string GetClockPinColor()
        {
            return $"mud-picker-time-clock-pin mud-{this.Color.ToDescriptionString()}";
        }

        private string GetClockPointerColor()
        {
            if (this.MouseDown)
            {
                return $"mud-picker-time-clock-pointer mud-{this.Color.ToDescriptionString()}";
            }

            return $"mud-picker-time-clock-pointer mud-picker-time-clock-pointer-animation mud-{this.Color.ToDescriptionString()}";
        }

        private string GetClockPointerThumbColor()
        {
            var deg = this.GetDeg();
            if (deg % 30 == 0)
            {
                return $"mud-picker-time-clock-pointer-thumb mud-onclock-text mud-onclock-primary mud-{this.Color.ToDescriptionString()}";
            }

            return $"mud-picker-time-clock-pointer-thumb mud-onclock-minute mud-{this.Color.ToDescriptionString()}-text";
        }

        private string GetNumberColor(int value)
        {
            if (this._currentView == OpenTo.Hours)
            {
                var h = this._timeSet.Hour;
                if (this.AmPm)
                {
                    h = this._timeSet.Hour % 12;
                    if (this._timeSet.Hour % 12 == 0)
                    {
                        h = 12;
                    }
                }

                if (h == value)
                {
                    return $"mud-clock-number mud-theme-{this.Color.ToDescriptionString()}";
                }
            }
            else if ((this._currentView == OpenTo.Minutes) && (this._timeSet.Minute == value))
            {
                return $"mud-clock-number mud-theme-{this.Color.ToDescriptionString()}";
            }

            return "mud-clock-number";
        }

        private double GetDeg()
        {
            double deg = 0;
            if (this._currentView == OpenTo.Hours)
            {
                deg = this._timeSet.Hour * 30 % 360;
            }

            if (this._currentView == OpenTo.Minutes)
            {
                deg = this._timeSet.Minute * 6 % 360;
            }

            return deg;
        }

        private string GetTransform(double angle, double radius, double offsetX, double offsetY)
        {
            angle = angle / 180 * Math.PI;
            var x = (Math.Sin(angle) * radius + offsetX).ToString("F3", CultureInfo.InvariantCulture);
            var y = ((Math.Cos(angle) + 1) * radius + offsetY).ToString("F3", CultureInfo.InvariantCulture);
            return $"transform: translate({x}px, {y}px);";
        }

        private string GetPointerRotation()
        {
            double deg = 0;
            if (this._currentView == OpenTo.Hours)
            {
                deg = this._timeSet.Hour * 30 % 360;
            }

            if (this._currentView == OpenTo.Minutes)
            {
                deg = this._timeSet.Minute * 6 % 360;
            }

            return $"rotateZ({deg}deg);";
        }

        private string GetPointerHeight()
        {
            var height = 40;
            if (this._currentView == OpenTo.Minutes)
            {
                height = 40;
            }

            if (this._currentView == OpenTo.Hours)
            {
                if (!this.AmPm && (this._timeSet.Hour > 0) && (this._timeSet.Hour < 13))
                {
                    height = 26;
                }
                else
                {
                    height = 40;
                }
            }

            return $"{height}%;";
        }

        private void UpdateTimeSetFromTime()
        {
            if (this.TimeIntermediate == null)
            {
                this._timeSet.Hour = 0;
                this._timeSet.Minute = 0;
                return;
            }

            this._timeSet.Hour = this.TimeIntermediate.Value.Hours;
            this._timeSet.Minute = this.TimeIntermediate.Value.Minutes;
        }

        /// <summary>
        ///     Sets Mouse Down bool to true if mouse is inside the clock mask.
        /// </summary>
        private void OnMouseDown(MouseEventArgs? _)
        {
            this.MouseDown = true;
        }

        private void OnTouchStart(TouchEventArgs obj)
        {
            this.OnMouseDown(null);
        }

        private void OnTouchEnd(TouchEventArgs obj)
        {
            this.OnMouseUp(null);
        }

        /// <summary>
        ///     Sets Mouse Down bool to false if mouse is inside the clock mask.
        /// </summary>
        private void OnMouseUp(MouseEventArgs? _)
        {
            this.MouseDown = false;
            if ((this._currentView == OpenTo.Hours) && (this._timeSet.Hour != this._initialHour) && (this.TimeEditMode == TimeEditMode.Normal))
            {
                this._currentView = OpenTo.Minutes;
            }
        }

        /// <summary>
        ///     If MouseDown is true enabels "dragging" effect on the clock pin/stick.
        /// </summary>
        private void OnMouseOverHour(int value)
        {
            if (this.MouseDown)
            {
                this._timeSet.Hour = value;
                this.UpdateTime();
            }
        }

        /// <summary>
        ///     On click for the hour "sticks", sets the houre.
        /// </summary>
        private void OnMouseClickHour(int value)
        {
            var h = value;
            if (this.AmPm)
            {
                if (this.IsAm && (value == 12))
                {
                    h = 0;
                }
                else if (this.IsPm && (value < 12))
                {
                    h = value + 12;
                }
            }

            this._timeSet.Hour = h;
            this.UpdateTime();

            if (this.TimeEditMode == TimeEditMode.Normal)
            {
                this._currentView = OpenTo.Minutes;
            }
        }

        /// <summary>
        ///     On click for the minutes "sticks", sets the minute.
        /// </summary>
        private void OnMouseOverMinute(int value)
        {
            if (this.MouseDown)
            {
                this._timeSet.Minute = value;
                this.UpdateTime();
            }
        }

        /// <summary>
        ///     On click for the minute "sticks", sets the minute.
        /// </summary>
        private void OnMouseClickMinute(int value)
        {
            this._timeSet.Minute = value;
            this.UpdateTime();
        }

        private async void CallJsInterop(TouchEventArgs eventArgs)
        {
            if ((this.getElementByPointsHelper != null) && eventArgs.Touches.FirstOrDefault() is { } touch)
            {
                Console.WriteLine($"We got the args, just fine {touch.ClientX} {touch.ClientY}");
                var mouseOverElementId = await this.getElementByPointsHelper.InvokeAsync<string>("documentElementFromPoint", touch.ClientX, touch.ClientY)
                                                   .AsTask();

                if (mouseOverElementId.StartsWith("workarounds_are_kind_of_annoying_", StringComparison.InvariantCulture))
                {
                    var intString = mouseOverElementId[(mouseOverElementId.LastIndexOf('_') + 1)..];

                    this.OnMouseOverMinute(int.Parse(intString, CultureInfo.InvariantCulture));
                }

                if (mouseOverElementId.StartsWith("hour_workarounds_are_kind_of_annoying_", StringComparison.InvariantCulture))
                {
                    var intString = mouseOverElementId[(mouseOverElementId.LastIndexOf('_') + 1)..];

                    this.OnMouseOverHour(int.Parse(intString, CultureInfo.InvariantCulture));
                }
            }
        }

        private class SetTime
        {
            public int Hour { get; set; }

            public int Minute { get; set; }
        }
    }

    internal static class TimeSpanExtensions
    {
        public static string ToIsoString(this TimeSpan self, bool seconds = false, bool ms = false)
        {
            if (!seconds)
            {
                return $"{self.Hours:D2}:{self.Minutes:D2}";
            }

            if (!ms)
            {
                return $"{self.Hours:D2}:{self.Minutes:D2}-{self.Seconds:D2}";
            }

            return $"{self.Hours:D2}:{self.Minutes:D2}-{self.Seconds:D2},{self.Milliseconds}";
        }

        public static string? ToIsoString(this TimeSpan? self, bool seconds = false, bool ms = false)
        {
            if (self == null)
            {
                return null;
            }

            return self.Value.ToIsoString(seconds, ms);
        }

        public static string ToAmPmString(this TimeSpan time, bool seconds = false)
        {
            var pm = time.Hours >= 12;
            var h = time.Hours % 12;
            if (h == 0)
            {
                h = 12;
            }

            if (!seconds)
            {
                return $"{h:D2}:{time.Minutes:D2} {(pm ? "PM" : "AM")}";
            }

            return $"{h:D2}:{time.Minutes:D2}{time.Seconds:D2} {(pm ? "PM" : "AM")}";
        }

        public static string? ToAmPmString(this TimeSpan? self, bool seconds = false)
        {
            if (self == null)
            {
                return null;
            }

            return self.Value.ToAmPmString(seconds);
        }

        public static int ToAmPmHour(this TimeSpan time)
        {
            var h = time.Hours % 12;
            if (h == 0)
            {
                h = 12;
            }

            return h;
        }
    }
}