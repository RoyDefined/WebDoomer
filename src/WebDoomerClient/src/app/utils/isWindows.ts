export function isWindows() {
    const match = navigator.userAgent.match(/(Windows)/i);
    return match != undefined && match.length > 0;
}
