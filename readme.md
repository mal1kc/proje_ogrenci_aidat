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
- [X] add Message tempData for all views
- [ ] add payment verification panel for sch admin (implement)
- [X] add payment panel for student (improve usability)
- [X] add workYear management panel for sch admin (improve usability)
- [X] add paymentperiod management panel for sch admin ( improve usability)
- [X] add all models to delete , edit , create panel for site admin (some of them must be debug only) +++
    - [X] payment
        - [-] edit (makePayment , verify payment)
        - [X] detail
        - [X] delete (only site admin (debug only))
        - [X] list
    - [X] paymentperiod (only school admin, site admin)
        - [-] edit
        - [X] create
        - [X] delete
        - [X] detail
        - [X] list
    - [X] workYear  (only school admin, site admin)
        - [-] edit
        - [X] create
        - [X] delete
        - [X] detail
        - [X] list
    - [X] student  (only school admin, site admin)
        - [-] edit
        - [X] create
        - [X] delete
        - [X] detail
        - [X] list
    - [X] school (only school admin, site admin)
        - [-] edit
        - [X] create   (only site admin)
        - [X] delete (only site admin)
        - [X] detail
        - [X] list (only site admin)
    - [X] schoolAdmin (only school admin, site admin)
        - [-] edit
        - [X] create
        - [X] delete
        - [X] detail
        - [X] list
    - [X] contactInfo (all , but student can only see their own)
        - [-] edit(integrated with student & school admin edit)
        - [X] create(integrated with student & school admin create)
        - [-] delete
        - [X] detail (not necessary because it can be shown with _contactInfo partial views)
        - [X] list (not necessary)
    - [X] receipt (all , but student can only see their own)
        - [X] delete
        - [X] detail
        - [X] list
    - [X] grade not necessary

- [X] crete paymentPeriod service for auto create payments for students and save to db
- [X] try to add school id as claim for school admin and student , and for site admin it should be 0 or null
- [X] rewrite ModelQueryableHelper to use predefined search methods and sort methods
- [X] implement exportService for exporting data to excel or similar formats
- [X] change errorController pages (404, 500, 403, 401)
    - [X] add error handling for all controllers and services

- [X] improve look by adding dark/light color-mode

- [X] remove _dependentSeeder , seeder not improved unless they break

- [X] fix signing redirection // Account/login -> Home/index

- [X] remove admin layout and use only _layout (not useful)

- [X] in listings fix role based column visibility (don't know how but it's gone now)

- [x] fix sorting ( broken prob: TryListOrFail method causing)
    - [X] to lowering cause sorting failure (santization)

- [ ] school admin issues
    - [X] dashboard
    - [ ] account detail

- [ ] student
    - [X] dashboard
        - [x] add a message to dashboard if has a debt to pay
    - [ ] account detail
    - [X] delete create button
        - [X] on receipt listing
        - [X] on paymentPeriod listing
    -  [X] delete delete button on all listings all panes avaliable
    - [X] fix receipt details
    - [X] all of period - end/start date is Invalid Date
- [ ] site admin issues
    - [X] dashboard
    - [X] can't access workYear create
    - [ ] account detail
    - [X] can't access paymentPeriod create
    - [ ] maybe add export controllers for data export
    - [X] fix delete button on receipts

- [ ] release
    - [ ] change admin pass
    - [ ] add second admin
    - [X] add max lenght checker before sanitize search queries

## todo , maybe

- [X] move partial views to shared folder + add partial views for all possible models (done 5-6 commits before check)
- [X] öğrenci id ile giriş yapma
- [ ] student email generator (not planned)
- [X] add predefined search filters linQ queries and save as search config ( rewrite most of ModelQueryableHelper) (done with 32c88528e3a6e5e80a90356764fb473ee0174a38 & bug fix 27fa0523681c093290ffa5d4d7804fd51c814b5f)

## todo , maybe fix issues

- [X] ( possible fix at [[./OgrenciAidatSistemi/Services/UserService.cs:220]] ) if a user is deleted, or not exist in db , if it is logged in, its still logged in, fix this
    - in this condition, user related panels but navbar is not shown , logged status is not shown ( not easily detectable)
- [X] list view links does not carry search parameters or sort parameters to next page (prob easy fix)
