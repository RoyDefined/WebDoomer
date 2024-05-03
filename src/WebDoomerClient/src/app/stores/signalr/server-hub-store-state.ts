import { Observable } from 'rxjs';

export interface ServerHubStoreState {
    loading: boolean;
    error: Error | null;
}
