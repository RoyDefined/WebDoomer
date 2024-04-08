import { Server } from '../../models/server';

export interface ServersStoreState {
    items: Server[];
    loading: boolean;
    error: Error | null;
}
