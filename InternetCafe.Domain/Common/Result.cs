using System;
using System.Collections.Generic;
using System.Linq;

namespace InternetCafe.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        public bool IsFailure
        {
            get { return !IsSuccess;}
        }

        public static Result Success()
        {
            return new Result(true, string.Empty);
        }

        public static Result Failure(string error) 
        { 
            return new Result(false, error); 
        }

        public static Result<T> Success<T>(T value) 
        { 
            return new Result<T>(value, true, string.Empty);
        }

        public static Result<T> Failure<T>(string error) 
        {
            return new Result<T>(default!, false, error);
        }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("A successful result cannot contain an error message");

            IsSuccess = isSuccess;
            Error = error;
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
        {
            Value = value;
        }
    }

    public class PagedResult<T> : Result
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        protected internal PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount, bool isSuccess, string error) 
            : base(isSuccess, error)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public static PagedResult<T> Success(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
        {
            return new PagedResult<T>(items, pageNumber, pageSize, totalCount, true, string.Empty);
        }

        public static PagedResult<T> Failure(string error)
        {
            return new PagedResult<T>(new List<T>().AsReadOnly(), 0, 0, 0, false, error);
        }
    }
}