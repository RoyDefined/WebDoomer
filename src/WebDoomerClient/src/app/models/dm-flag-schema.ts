import { z } from 'zod';

export const dmFlagSchema = z
    .number()
    .nonnegative()
    .catch(() => 0);
