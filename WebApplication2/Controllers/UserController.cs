using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.Controllers
{

    public class UserViewModel
    {
        public Guid? Id { get; set; }
        public string Login { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public string Title { get; set; }
        public string ErrorMessage { get; set; }
        public bool CreationMode { get; set; }
    }

    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly Mapper _mapper;

        public UserController(IUserService service)
        {
            _userService = service;
            _mapper = new Mapper();
        }

        public IActionResult Index()
        {
            return View(new UserViewModel() { Title = "Create a new user", CreationMode = true });
        }

        public IActionResult Cancel()
        {
            return RedirectToPage("/ListOfUsers");
        }

        [HttpGet("/{userId}")]
        public IActionResult Index(Guid userId)
        {
            try
            {
                return View(_mapper.UserToViewModelMapper(_userService.Get(userId), false, "Edit a user"));
            }
            catch (Exception ex)
            {
                return View(new UserViewModel() { ErrorMessage = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult Save(UserViewModel userViewModel)
        {
            try
            {
                if (userViewModel.Login == null)
                    throw new ArgumentNullException("Login", "Invalid data!");

                if (userViewModel.Id != null)
                {
                    bool updated = _userService.Update(_mapper.ViewModelToUserMapper(userViewModel));
                    if (!updated)
                        throw new ArgumentException("Could not be updated");
                }
                else
                {
                    _userService.Create(_mapper.ViewModelToUserMapper(userViewModel));
                }

                return RedirectToPage("/Confirmation");
            }
            catch (Exception ex)
            {
                return View("Index", new UserViewModel() { ErrorMessage = ex.Message });
            }
        }
        public IActionResult ListOfUsers()
        {
            return View();
        }
        public IActionResult Confirmation()
        {
            return View();
        }
    }

    public interface IUserService
    {
        Guid Create(User user);

        User Get(Guid id);

        bool Update(User user);
    }

    public class UserService : IUserService
    {
        public List<User> usersInMemory => new List<User>
        { 
            new User { id = new Guid("c722ac5b-88e7-4b0d-b95c-34f87da11c9c") , firstName = "FirstUser" , lastName = "FirstLastName", login = "FirstLogin" }
        };

        public User Get(Guid id)
        {
            User user = usersInMemory.FirstOrDefault(item => item.id == id);

            if(user == null)
            {
                throw new ArgumentNullException("User", "User" + "id" + "was not found!");
            }

            return user;
        }

        public bool Update(User user)
        {
            try
            {

                User updatedUser = Get(user.id);

                if (updatedUser != null)
                {
                    updatedUser.firstName = user.firstName;
                    updatedUser.lastName = user.lastName;
                    updatedUser.login = user.login;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Guid Create(User user)
        {
            user.id = Guid.NewGuid();
            usersInMemory.Add(user);

            return user.id;
        }
    }

    public class User
    {
        public Guid id { get; set; }

        public string login { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }
    }

    public class Mapper{
        public UserViewModel UserToViewModelMapper(User user, bool creationMode, string title) 
        {
            return new UserViewModel() { Id = user.id, Firstname = user.firstName, Lastname = user.lastName, Login = user.login, CreationMode = creationMode, ErrorMessage = "", Title = title};
        }
        public User ViewModelToUserMapper(UserViewModel userViewModel)
        {
            return new User() { id = userViewModel.Id == null ? Guid.Empty : (Guid)userViewModel.Id, firstName = userViewModel.Firstname, lastName = userViewModel.Lastname, login = userViewModel.Login };
        }
    }
}
