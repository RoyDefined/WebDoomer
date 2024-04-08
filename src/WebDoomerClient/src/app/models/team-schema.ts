import { z } from 'zod';

export const teamSchema = z
    .object({
        name: z.string().min(1).optional().nullable(),
        color: z.number().nonnegative().optional().nullable(),
        score: z.number().nonnegative().optional().nullable(),
    })
    .strict();
