export interface PingStoreState {
    ping: number | null;
    loading: boolean;
    error: Error | null;
}
