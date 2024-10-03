import { z } from 'zod';

export const pwadSchema = z
    .object({
        name: z
            .string()
            .min(1)
            .catch(() => 'N/A'),
        optional: z.boolean().optional().nullable(),
        hash: z
            .string()
            .min(1)
            .optional()
            .nullable()
            .catch(() => null),
    })
    .strict();
