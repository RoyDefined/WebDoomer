import { z } from 'zod';
import { playerSchema } from './player-schema';

export type player = z.infer<typeof playerSchema>;
