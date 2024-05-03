import { Injectable, TemplateRef } from '@angular/core';

// Service that handles passing references between components.
@Injectable({
    providedIn: 'root',
})
export class HeaderRefService {
    public leftRef?: TemplateRef<unknown>;
    public rightRef?: TemplateRef<unknown>;
    public bottomRef?: TemplateRef<unknown>;
}
