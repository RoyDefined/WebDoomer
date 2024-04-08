import { z } from 'zod';
import { clientSettingsSchema } from './client-settings-schema';

export interface ClientSettingsStoreState {
    items: z.infer<typeof clientSettingsSchema>;
}
