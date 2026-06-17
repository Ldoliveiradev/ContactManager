import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';

@Component({
  selector: 'app-server-error-page',
  imports: [RouterLink, ButtonComponent, CardComponent],
  templateUrl: './server-error-page.component.html',
  styleUrl: './server-error-page.component.scss',
})
export class ServerErrorPageComponent {}
