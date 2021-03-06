﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EpamLibrary.BLL.Interfaces;
using EpamLibrary.Contracts.Models;
using EpamLibrary.DAL.Interfaces;

namespace EpamLibrary.BLL.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<Author> _authorRepository;

        public BookService(
            IUnitOfWork unitOfWork,
            IRepository<Book> bookRepository,
            IRepository<Author> authorRepository)
        {
            _unitOfWork = unitOfWork;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }
        
        public void AddBook(Book book)
        {
            _bookRepository.Create(book);

            if (book.Authors != null)
            {
                foreach (var bookAuthor in book.Authors)
                {
                    var author = bookAuthor;
                    author.Books.Add(book);

                    _authorRepository.Update(author);
                }
            }
            _unitOfWork.Save();
        }

        public void RemoveBook(int bookId)
        {
            _bookRepository.Delete(bookId);
            _unitOfWork.Save();
        }

        public Book GetBook(int bookId)
        {
            return _bookRepository.GetById(bookId);
        }

        ///<exception cref="ArgumentException"></exception>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="from"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetBooks(
            Expression<Func<Book, bool>> predicate = null,
            int @from = 0,
            int count = 10)
        {
            var books = _bookRepository.Get(predicate);
            if (books == null)
                return null;

            if (from > books.Count())
            {
                throw new ArgumentException("From parametr greater then count of books.");
            }

            var rest = books.Count() - @from;
            if (rest < count)
            {
                count = rest;
            }

            var enumerable = books.Skip(@from).Take(count).ToList();
            return enumerable;
        }

        public IEnumerable<Book> GetTopBooks(int count)
        {
            var books = _bookRepository.Get();
            books = books.OrderBy(b => b.BookReviews?.Sum(r => (int) r.Rating) / b.BookReviews?.Count);

            return books.Take(count);
        }

        public IEnumerable<Book> GetLastReviewedBooks()
        {
            var enumerable = _bookRepository.Get().OrderByDescending(b =>
            {
                return b.BookReviews?.OrderByDescending(r => r.PublicationDateTime).First().PublicationDateTime;
            }).Take(4);
            return enumerable;
        }

        public IEnumerable<Book> GetNewestBooks()
        {
            var books = _bookRepository.Get().OrderByDescending(b => b.Id).Take(4);
            return books;
        }

        public IEnumerable<Book> GetBy(string name = null, IEnumerable<Tag> tags = null, IEnumerable<Genre> genres = null)
        {
            var byName = _bookRepository.Get(b => b.Title.Contains(name));
            var strTags = tags?.Select(t => t.TagName).ToList();
            //var byTag = byName.Where(b => b.Tags.Select(s => strTags.Contains(s.TagName)));
            //TODO: search by tags and genres

            return byName;
        }

        public void UpdateBook(Book book)
        {
            _bookRepository.Update(book);
        }
    }
}
