using System;
using System.Collections.Generic;
using Auction.mod;
namespace Auction.mod
{
	public class AucItemFilter
	{
		/// <summary>
		/// Method removes everything from the list, that does not match the filter.
		/// </summary>
		/// <param name="l">List to filter</param>
		/// <param name="filter">Returns true for everything that should stay in the list.</param>
		public static void filterList(List<aucitem> l, filterAuc filter) {
			l.RemoveAll (new Predicate<aucitem> ((aucitem a) => (!filter(a))));
		}
		public static List<aucitem> getFilteredList(List<aucitem> l, filterAuc filter) {
			List<aucitem> filtered = new List<aucitem>();
			foreach(aucitem a in l) {
				if (filter(a)) {
					filtered.Add (a);
				}
			}
			return filtered;
		}
	}

	public delegate bool filterAuc(aucitem a);



}

