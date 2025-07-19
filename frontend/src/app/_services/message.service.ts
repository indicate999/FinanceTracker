import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = 'http://localhost:5258/api/test';
  private apiHttpsUrl = 'https://localhost:7204/api/test';
  constructor(private http: HttpClient) { }

  getMessage(): Observable<string> {
    return this.http.get('/api/test', { responseType: 'text' });
  }
}
