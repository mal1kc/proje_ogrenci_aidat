using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Tests
{
    public class UserViewValidationTests
    {
        [Fact]
        public void ValidateFieldsSignUp_PasswordsNotMatch_ReturnsPasswordsNotMatch()
        {
            // Arrange
            var userView = new TestUserView
            {
                Password = "password123",
                PasswordVerify = "password456"
            };

            // Act
            var result = userView.ValidateFieldsSignUp(null);

            // Assert
            Assert.Equal(UserViewValidationResult.PasswordsNotMatch, result);
        }

        [Fact]
        public void ValidateFieldsSignUp_InvalidName_ReturnsInvalidName()
        {
            // Arrange
            var userView = new TestUserView
            {
                Password = "password123",
                PasswordVerify = "password123",
                FirstName = "A",
                LastName = new string('B', Constants.MaxUserNameLength + 1)
            };

            // Act
            var result = userView.ValidateFieldsSignUp(null);

            // Assert
            Assert.Equal(UserViewValidationResult.InvalidName, result);
        }

        [Fact]
        public void ValidateFieldsSignUp_EmailAddressNotMatchRegex_ReturnsEmailAddressNotMatchRegex()
        {
            // Arrange
            var userView = new TestUserView
            {
                Password = "password123",
                PasswordVerify = "password123",
                FirstName = "John",
                EmailAddress = "invalid-email"
            };

            // Act
            var result = userView.ValidateFieldsSignUp(null);

            // Assert
            Assert.Equal(UserViewValidationResult.EmailAddressNotMatchRegex, result);
        }

        [Fact]
        public void ValidateFieldsSignIn_EmailAddressNotMatchRegex_ReturnsEmailAddressNotMatchRegex()
        {
            // Arrange
            var userView = new TestUserView
            {
                EmailAddress = "invalid-email",
                Password = "password123"
            };

            // Act
            var result = userView.ValidateFieldsSignIn();

            // Assert
            Assert.Equal(UserViewValidationResult.EmailAddressNotMatchRegex, result);
        }

        [Fact]
        public void ValidateFieldsSignIn_PasswordEmpty_ReturnsPasswordEmpty()
        {
            // Arrange
            var userView = new TestUserView { EmailAddress = "test@example.com", Password = "" };

            // Act
            var result = userView.ValidateFieldsSignIn();

            // Assert
            Assert.Equal(UserViewValidationResult.PasswordEmpty, result);
        }

        [Fact]
        public void ValidateFieldsCreate_PasswordsNotMatch_ReturnsPasswordsNotMatch()
        {
            // Arrange
            var userView = new TestUserView
            {
                Password = "password123",
                PasswordVerify = "password456"
            };

            // Act
            var result = userView.ValidateFieldsCreate(null);

            // Assert
            Assert.Equal(UserViewValidationResult.PasswordsNotMatch, result);
        }

        [Fact]
        public void ValidateFieldsCreate_PasswordEmpty_ReturnsPasswordEmpty()
        {
            // Arrange
            var userView = new TestUserView { Password = "", PasswordVerify = "" };

            // Act
            var result = userView.ValidateFieldsCreate(null);

            // Assert
            Assert.Equal(UserViewValidationResult.PasswordEmpty, result);
        }

        [Fact]
        public void ValidateFieldsCreate_InvalidName_ReturnsInvalidName()
        {
            // Arrange
            var userView = new TestUserView
            {
                Password = "password123",
                PasswordVerify = "password123",
                FirstName = "A",
                LastName = new string('B', Constants.MaxUserNameLength + 1)
            };

            // Act
            var result = userView.ValidateFieldsCreate(null);

            // Assert
            Assert.Equal(UserViewValidationResult.InvalidName, result);
        }

        // Helper class to test the abstract UserView class
        private class TestUserView : UserView
        {
            public override bool CheckUserExists(AppDbContext dbctx) => false;

            public override bool CheckEmailAddressExists(AppDbContext dbctx) => false;

            public override bool Equals(object? obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string? ToString()
            {
                return base.ToString();
            }

            public override UserViewValidationResult ValidateFieldsSignIn()
            {
                return base.ValidateFieldsSignIn();
            }
        }
    }
}
