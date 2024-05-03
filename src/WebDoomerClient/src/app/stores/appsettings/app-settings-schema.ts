import { z } from 'zod';

/**
 * Represents the base schema used for the ping protocol schema.
 */
export const appSettingsPingProtocolSchema = z.union([z.literal('Unix'), z.literal('Timer')]);

/**
 * Represents the base schema used to define the app settings file.
 */
export const appSettingsSchema = z
    .object({
        apiBaseUrl: z.string().min(1),
        pingProtocol: appSettingsPingProtocolSchema,
    })
    .strict();
