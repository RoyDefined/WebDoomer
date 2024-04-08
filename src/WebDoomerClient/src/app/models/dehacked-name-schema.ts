import { z } from 'zod';

export const dehackedNameSchema = z.string().min(1);
