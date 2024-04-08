import { z } from 'zod';

/**
 * Represents the base schema used to define the registery file response.
 */
export const registryFileSchema = z.string().min(1);
