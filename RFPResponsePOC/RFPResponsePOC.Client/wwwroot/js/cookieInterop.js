// Cookie helpers for GuideFlow first-run detection.
// Exposed as an ES module imported on demand by TourService.

export function getCookie(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? decodeURIComponent(match[2]) : null;
}

export function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    const secure = location.protocol === 'https:' ? '; Secure' : '';
    document.cookie =
        name + '=' + encodeURIComponent(value) +
        '; expires=' + expires + '; path=/; SameSite=Lax' + secure;
}

export function deleteCookie(name) {
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
}
