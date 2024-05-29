# öğrenci aidat sistemi projesi 2024

## özellikler

- admin paneli
- okul admin yönetim paneli
- okul çalışma yılı yönetimi (workYear)
- aidat dönemi yönetimi (single payment period)
- öğrenci yönetimi
- öğrenci bazlı ödeme planı yönetimi (paymentperiod)
- excel export / import
- dekont depolama

## todo

- [X] studentId generator
- [x] add Message tempData for all views
- [ ] implement dashboards for all roles
    - [ ] site admin dashboard
    - [ ] school admin dashboard
    - [X] student dashboard
- [ ] add payment verification panel for sch admin (implement)
- [X] add payment panel for student (improve usability)
- [ ] add workYear management panel for sch admin (improve usability)
- [ ] add paymentperiod management panel for sch admin ( improve usability)
- [ ] add all models to delete , edit , create panel for site admin (some of them must be debug only) +++

    - [ ] payment
        - [ ] edit (makePayment , verify payment)
        - [x] detail
        - [x] delete (only site admin (debug only))
        - [x] list
    - [ ] paymentperiod (only school admin, site admin)
        - [ ] edit  
        - [x] create
        - [X] delete
        - [X] detail
        - [x] list
    - [ ] workYear  (only school admin, site admin)
        - [ ] edit  
        - [ ] create (needs re-look)
        - [X] delete
        - [X] detail
        - [x] list  
    - [ ] student  (only school admin, site admin)
        - [ ] myProfile
        - [ ] edit
        - [X] create
        - [X] delete
        - [x] detail
        - [x] list 
    - [ ] school (only school admin, site admin)
        - [ ] edit
        - [x] create   (only site admin)
        - [x] delete (only site admin)
        - [x] detail
        - [x] list (only site admin)
    - [ ] schoolAdmin (only school admin, site admin)
        - [ ] myProfile
        - [ ] edit
        - [x] create 
        - [x] delete
        - [X] detail
        - [x] list
    - [ ] contactInfo (all , but student can only see their own)
        - [ ] edit(integrated with student & school admin edit)
        - [X] create(integrated with student & school admin create)
        - [ ] delete
        - [X] detail (not necessary because it can be shown with _contactInfo partial views) 
        - [X] list (not necessary)
    - [ ] receipt (all , but student can only see their own)
        - [ ] edit (integrated with payment)
        - [ ] create (integrated with payment)
        - [ ] delete
        - [X] detail
        - [X] list
    - [X] grade not necessary

- [X] crete paymentPeriod service for auto create payments for students and save to db
- [X] try to add school id as claim for school admin and student , and for site admin it should be 0 or null
- [X] rewrite ModelQueryableHelper to use predefined search methods and sort methods
- [x] implement exportService for exporting data to excel or similar formats
- [x] change errorController pages (404, 500, 403, 401) 
    - [ ] add error handling for all controllers and services

- [x] improve look by adding dark/light color-mode

- [X] remove _dependentSeeder , seeder not improved unless they break

- [X] fix signing redirection // Account/login -> Home/index

- [X] remove admin layout and use only _layout (not useful)

- [ ] ordered pagination not seems to work as expected (fix)

- [ ] in listings fix role based column visibility (not working as expected)

## todo , maybe

- [X] move partial views to shared folder + add partial views for all possible models (done 5-6 commits before check)
- [X] öğrenci id ile giriş yapma 
- [ ] student email generator (not planned)
- [X] add predefined search filters linQ queries and save as search config ( rewrite most of ModelQueryableHelper) (done with 32c88528e3a6e5e80a90356764fb473ee0174a38 & bug fix 27fa0523681c093290ffa5d4d7804fd51c814b5f)

## todo , maybe fix issues

- [X] ( possible fix at [[./OgrenciAidatSistemi/Services/UserService.cs:220]] ) if a user is deleted, or not exist in db , if it is logged in, its still logged in, fix this
    - in this condition, user related panels but navbar is not shown , logged status is not shown ( not easily detectable)
- [X] list view links does not carry search parameters or sort parameters to next page (prob easy fix)