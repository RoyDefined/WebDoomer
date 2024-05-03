import { z } from 'zod';

export const playerSchema = z
    .object({
        name: z.string().min(1),
        scoreCount: z.number(),
        ping: z.number().nonnegative().nullable(),
        isSpectating: z.boolean(),
        isBot: z.boolean(),
        team: z.number().nonnegative().nullable(),
        playTime: z.number().nonnegative(),
    })
    .strict();
