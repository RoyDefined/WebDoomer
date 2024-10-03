import { z } from 'zod';

export const playerSchema = z
    .object({
        name: z
            .string()
            .min(1)
            .catch(() => 'N/A'),
        scoreCount: z.number(),
        ping: z
            .number()
            .nonnegative()
            .nullable()
            .catch(() => null),
        isSpectating: z.boolean(),
        isBot: z.boolean(),
        team: z
            .number()
            .nonnegative()
            .nullable()
            .catch(() => null),
        playTime: z.number().nonnegative(),
    })
    .strict();
