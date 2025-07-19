import { Component, OnInit } from '@angular/core';
import { MessageService } from '../../_services/message.service';

@Component({
  selector: 'app-message',
  standalone: true,
  imports: [],
  templateUrl: './message.component.html',
  styleUrl: './message.component.scss'
})
export class MessageComponent implements OnInit{
  message: string = '';

  constructor(private messageService: MessageService) {}

  ngOnInit(): void {
    this.messageService.getMessage().subscribe({
      next: (data: string) => this.message = data,
      error: (err: Error) => console.error(err)
    });
  }
}
