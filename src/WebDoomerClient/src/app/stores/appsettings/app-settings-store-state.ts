import { z } from 'zod';
import { appSettingsSchema } from './app-settings-schema';

export interface AppSettingsStoreState {
    item: z.infer<typeof appSettingsSchema> | null;
    loading: boolean;
    error: Error | null;
}
