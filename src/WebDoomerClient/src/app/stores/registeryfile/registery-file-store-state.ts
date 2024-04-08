export interface RegisteryFileStoreState {
    baseItem: string | null;
    parsedItem: string | null;
    loading: boolean;
    error: Error | null;
}
