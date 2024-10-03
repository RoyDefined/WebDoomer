import { z } from 'zod';

export const teamSchema = z
    .object({
        name: z
            .string()
            .min(1)
            .optional()
            .nullable()
            .catch(() => null),
        color: z
            .number()
            .nonnegative()
            .optional()
            .nullable()
            .catch(() => null),
        score: z
            .number()
            .nonnegative()
            .optional()
            .nullable()
            .catch(() => null),
    })
    .strict();
