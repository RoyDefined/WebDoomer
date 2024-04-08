/**
 * The fetch state of the server.
 */
export type ServerFetchState =
    // Only the id was fetched.
    | 'id'

    // The server contains info for in a list.
    | 'list'

    // The server contains all info.
    | 'detailed';
