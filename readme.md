# öğrenci aidat sistemi projesi 2024

## özellikler

- admin paneli
- okul admini yönetim paneli
- okul çalışma yılı yönetimi (workyear)
- aidat dönemi yönetimi (single payment period)
- öğrenci yönetimi
- öğrenci bazlı ödeme plani yönetimi (paymentperiod)
- excel export / import
- dekont depolama

## todo

- [X] studentid generator
- [x] add Message tempdata for all views
- [ ] add payment verification panel for sch admin
- [ ] add payment panel for student
- [ ] add workyear management panel for sch admin
- [ ] add paymentperiod management panel for sch admin (create, edit, delete) ++
- [ ] add all models to delete , edit , create panel for site admin (some of them must be debug only) +++

    - [ ] payment
        - [ ] edit (only school admin (only payment verification))
        - [ ] create (only student)
        - [x] detail
        - [x] delete (only site admin (debug only))
        - [x] list
    - [ ] paymentperiod (only school admin, site admin)
        - [ ] edit  
        - [x] create
        - [X] delete
        - [X] detail
        - [x] list
    - [ ] workyear  (only school admin, site admin)
        - [ ] edit  
        - [ ] create (needs relook)
        - [X] delete
        - [X] detail
        - [x] list  
    - [ ] student  (only school admin, site admin)
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
    - [ ] schooladmin (only school admin, site admin)
        - [ ] edit
        - [x] create 
        - [x] delete
        - [X] detail
        - [x] list
    - [ ] contactınfo (all , but student can only see their own)
        - [ ] edit(integrated with student & school admin edit)
        - [X] create(integrated with student & school admin create)
        - [ ] delete
        - [X] detail (not necessary because it can be shown with _contactinfo partial views) 
        - [X] list (not necessary)
    - [ ] receipt (all , but student can only see their own)
        - [ ] edit (integrated with payment)
        - [ ] create (integrated with payment)
        - [ ] delete
        - [X] detail
        - [X] list
    - [X] grade not necessary

- [ ] add services for retrieving related data for students and school admins - (studentService, schoolAdminService)
- [ ] crete paymentperiod service for auto create payments for students and save to db 
- [X] try to add school id as claim for school admin and student , and for site admin it should be 0 or null
- [X] rewrited ModelQueryableHelper to use predefined search methods and sort methods
- [ ] impelment exportService for exporting data to excel or similiar formats
- [ ] change error's to use partial view for error pages (404, 500, 403, 401) 
    - [ ] add error handling for all controllers and services

- [ ] remove _dependentSeeder , seeder not improved unless they break

## todo , maybe

- [ ] move partial views to shared folder + add partial views for all possible models, add them to partialview controller or their model controller
- [X] öğrenci id ile giriş yapma 
- [ ] student email generator
- [ ] add predefined search filters linq queries and save as search config ( rewrite most of ModelQueryableHelper)

## todo , maybe fix issues

- [X] ( possible fix at [[./OgrenciAidatSistemi/Services/UserService.cs:220]] ) if a user is deleted, or not exist in db , if it is logged in, its still logged in, fix this
    - in this condition, user related panels but novbar is not shown , logged status is not shown ( not easylly detectable)
- [ ] list view links does not carry search parameters or sort parameters to next page (prob easy fix)