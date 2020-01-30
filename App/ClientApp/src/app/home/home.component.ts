import { Component, OnInit,  EventEmitter, Output} from '@angular/core';
import { DecodeHtmlEntitiesModule} from 'decode-html-entities';
import { ApiService } from '../api.service';
import {StripHtmlPipe} from '../../../pipes/striphtml.pipe';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  providers: [ StripHtmlPipe ]
})

export class HomeComponent implements OnInit {

  public results;
  public searchValue;
  public feedbackValue;
  public temporalRange;
  public feedbackLoV;

  public titulo = 'JP';

  constructor(private apiService: ApiService) { 
    this.feedbackLoV = ['Positive', 'Negative', 'All'];
  }

  ngOnInit() {

  }

  onSubmit(searchValue: string) {
    this.searchValue = searchValue;
    this.apiService.getDocuments(searchValue).subscribe((data)=>{
      this.results = data;
    });
  }


}
