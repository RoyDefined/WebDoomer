import { z } from 'zod';

/**
 * A schema for the possible darkmode values.
 */
export const darkModeTypeSchema = z.enum(['System', 'Light', 'Dark']);
