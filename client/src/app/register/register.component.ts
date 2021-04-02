import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  //model: any = {};
  registerForm: FormGroup;
  maxDate: Date;// dodano zbog kalendara da prikazuje datume starije od 18 god
  validationErrors: string[] = [];

  constructor(private accountService : AccountService, private toastr: ToastrService,
    private fb: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  /*initializeForm() {
    this.registerForm = new FormGroup({
      username: new FormControl('', Validators.required),
      password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
      confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')]) 
    })
    // if you change password after validate confirmPassword:
    // confirmPassword validator is aplied only to confirmPassword field,
    // we need to check the password field again and update isValid
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  } */
  // Uvodimo fb FormBuilder, pa metoda izgleda ovako:
  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]] 
    })
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }   
  
  // what is return type - ValidatorFn 
  matchValues(matchTo: string): ValidatorFn {
    // why AbstractControl - all our FormControls derive from AbstractControl
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo].value ? null : {isMatching: true}
      //     confirmPassword                              password          ok     dodajemo validatorError
    }
  }

  register() {
    //console.log(this.registerForm.value);
    //this.accountService.register(this.model).subscribe(response => {
    this.accountService.register(this.registerForm.value).subscribe(response => {
      //console.log(response);
      this.router.navigateByUrl('/members');
      //this.cancel();
    }, error => {
      //console.log(error);
      //this.toastr.error(error.error);// Komentiramo ga jer će bad request doći sa Interceptora
      this.validationErrors = error;
    })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
