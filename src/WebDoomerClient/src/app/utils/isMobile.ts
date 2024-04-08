export function isMobile() {
    const match = navigator.userAgent.match(/(iPad)|(iPhone)|(iPod)|(android)|(webOS)/i);
    return match != undefined && match.length > 0;
}
