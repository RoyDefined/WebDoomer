/**
 * Returns a number whose value is limited to the given range.
 * @param current The current value.
 * @param min The minimal possible value.
 * @param max The maximum possible value.
 * @returns The updated value.
 */
export function clamp(current: number, min: number, max: number) {
    return Math.min(Math.max(current, min), max);
}
