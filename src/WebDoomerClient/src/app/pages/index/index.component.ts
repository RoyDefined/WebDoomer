import { Component, OnInit } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ServersApiService } from '../../services/api/servers-api.service';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { ServersStore } from '../../stores/servers/servers.store';
import { RouterModule } from '@angular/router';

@Component({
    standalone: true,
    templateUrl: './index.component.html',
    host: {
        class: 'flex flex-col grow',
    },
    imports: [CommonModule, ScrollingModule, RouterModule, NgOptimizedImage],
    providers: [ServersStore],
})
export class IndexComponent {}
