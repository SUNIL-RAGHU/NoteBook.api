using System;
namespace Notebook.DataService.IRepository
{
	public interface IGenericRepository<T> where T:class
	{
		//Get all entities
		Task<IEnumerable<T>> All();

		//Get specific entity by id

		Task<T> GetbyId(Guid id);


		Task<bool> Add(T entity);

		Task<bool> Delete(Guid id, string userId);


		//update entity or add does not exist

		Task<bool> Upsert(T entity); 
	}
}

