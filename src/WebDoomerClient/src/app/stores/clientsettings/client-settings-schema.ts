import { z } from 'zod';
import { darkModeTypeSchema } from './darkModeType';

/**
 * Represents the base schema used to define client settings.
 */
export const clientSettingsSchema = z
    .object({
        darkModeType: darkModeTypeSchema.optional().nullable(),
        zandronumLocation: z.string().optional().nullable(),
        qZandronumLocation: z.string().optional().nullable(),
        iwadsLocation: z.string().optional().nullable(),
        pwadsLocation: z.string().optional().nullable(),
        doomseekerLocation: z.string().optional().nullable(),
    })
    .strict();
