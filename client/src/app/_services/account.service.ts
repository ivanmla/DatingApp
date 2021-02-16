import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any){
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user){
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    )
  }

  register(model : any){
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if(user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    )
  }

  setCurrentUser(user: User){
    this.currentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
/*
Q: if I log in as Lisa and then logout the Lisa and password still exists...

A: this can be fixed easily enough by resetting the form after you have successfully logged in.  
Since the login form is a template form you would need to use the @ViewChild directive to get 
access to the form itself, then in the component after the successful login you can simply call 
form.reset(), something like the following:

@ViewChild('loginForm') loginForm: NgForm

Then in the login method:
  login() {
    this.accountService.login(this.model).subscribe(response => {
      this.loginForm.reset();
      this.router.navigateByUrl('/members');
    })
  }

Thanks,
Neil
*/