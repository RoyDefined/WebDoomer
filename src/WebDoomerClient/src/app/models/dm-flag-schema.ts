import { z } from 'zod';

export const dmFlagSchema = z.number().nonnegative();
