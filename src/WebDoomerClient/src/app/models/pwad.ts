import { z } from 'zod';
import { pwadSchema } from './pwad-schema';

export type pwad = z.infer<typeof pwadSchema>;
