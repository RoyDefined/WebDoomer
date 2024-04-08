import { z } from 'zod';
import { teamSchema } from './team-schema';

export type team = z.infer<typeof teamSchema>;
