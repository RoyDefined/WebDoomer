/**
 * Formats the given string using the given replacements.
 * @param baseString The base string to format.
 * @param replacements The collection of replacements to replace the string with.
 * @returns The updated string.
 */
export function formatString(baseString: string, ...replacements: string[]) {
    for (let index = 0; index < replacements.length; index++) {
        baseString = baseString.replaceAll(`{${index}}`, replacements[index]);
    }

    return baseString;
}
