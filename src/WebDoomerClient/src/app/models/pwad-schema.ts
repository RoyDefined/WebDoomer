import { z } from 'zod';

export const pwadSchema = z
    .object({
        name: z.string().min(1),
        optional: z.boolean().optional().nullable(),
        hash: z.string().min(1).optional().nullable(),
    })
    .strict();
