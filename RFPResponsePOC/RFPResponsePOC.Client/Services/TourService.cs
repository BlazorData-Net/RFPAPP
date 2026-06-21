using System;
using System.Threading.Tasks;
using GuideFlow.Components;
using Microsoft.JSInterop;

namespace RFPResponseAPP.Client.Services
{
    /// <summary>
    /// Coordinates the GuideFlow animated onboarding tour:
    /// - Detects first-time visitors via a browser cookie.
    /// - Auto-starts the tour only on the very first visit, once the Home page is ready.
    /// - Allows the tour to be replayed on demand from the Help button.
    /// - Persists completion so the tour never auto-starts again.
    /// </summary>
    public class TourService
    {
        private const string CookieName = "rfpapp_first_run";
        private const int CookieDays = 365;

        private readonly IJSRuntime _js;
        private IJSObjectReference _module;
        private GuideFlowTour _tour;

        private bool _shouldAutoStart;
        private bool _homeReady;
        private bool _autoStarted;

        public TourService(IJSRuntime js) => _js = js;

        /// <summary>Called by MainLayout once the tour component is rendered.</summary>
        public void Attach(GuideFlowTour tour) => _tour = tour;

        private async Task<IJSObjectReference> ModuleAsync() =>
            _module ??= await _js.InvokeAsync<IJSObjectReference>(
                "import", "./js/cookieInterop.js");

        /// <summary>
        /// Checks the first-run cookie. If absent, flags the tour to auto-start
        /// as soon as the Home page reports that it is ready.
        /// </summary>
        public async Task MaybeStartForFirstRunAsync()
        {
            try
            {
                var module = await ModuleAsync();
                var seen = await module.InvokeAsync<string>("getCookie", CookieName);
                _shouldAutoStart = string.IsNullOrEmpty(seen);
                await TryAutoStartAsync();
            }
            catch (Exception)
            {
                // If cookie access fails (e.g. cookies disabled), skip auto-start silently.
            }
        }

        /// <summary>Called by Home once it has finished loading its menu and content.</summary>
        public async Task NotifyHomeReadyAsync()
        {
            _homeReady = true;
            await TryAutoStartAsync();
        }

        private async Task TryAutoStartAsync()
        {
            if (_autoStarted || !_shouldAutoStart || !_homeReady || _tour is null)
            {
                return;
            }

            _autoStarted = true;
            await _tour.DriveAsync();
        }

        /// <summary>Manually (re)starts the tour, regardless of the cookie.</summary>
        public async Task ReplayAsync()
        {
            if (_tour is not null)
            {
                await _tour.DriveAsync();
            }
        }

        /// <summary>Marks the tour as seen so it will not auto-start again.</summary>
        public async Task MarkCompletedAsync()
        {
            try
            {
                var module = await ModuleAsync();
                await module.InvokeVoidAsync("setCookie", CookieName, "completed", CookieDays);
            }
            catch (Exception)
            {
                // Non-fatal: if we cannot persist, the tour may show again next visit.
            }
        }
    }
}
