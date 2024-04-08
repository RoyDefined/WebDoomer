import { z } from 'zod';

/**
 * Represents the fetch result of a single entry in a list of server ids.
 */
export const serverIdListSchema = z.string();

export const serverIdListArraySchema = serverIdListSchema.array();
