import { z } from 'zod';

/**
 * Represents the base schema used to define the app settings file.
 */
export const appSettingsSchema = z
    .object({
        apiBaseUrl: z.string().min(1),
    })
    .strict();
