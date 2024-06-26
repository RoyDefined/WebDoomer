import { Server } from '../../models/server';

export interface ServersStoreState {
    items: Server[];
    loading: boolean;
    searchString: string | null;
    error: Error | null;
}
