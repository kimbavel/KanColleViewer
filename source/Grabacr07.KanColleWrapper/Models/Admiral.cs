﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// プレイヤー (提督) を表します。
	/// </summary>
	public class Admiral : RawDataWrapper<kcsapi_basic>
	{
		public string MemberId => this.RawData.api_member_id;

		public string Nickname => this.RawData.api_nickname;

		#region Comment 変更通知プロパティ

		private string _Comment;

		public string Comment
		{
			get { return this._Comment; }
			set
			{
				if (this._Comment != value)
				{
					this._Comment = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		/// <summary>
		/// 提督経験値を取得します。
		/// </summary>
		public int Experience => this.RawData.api_experience;

		/// <summary>
		/// 次のレベルに上がるために必要な提督経験値を取得します。
		/// </summary>
		public int ExperienceForNexeLevel => Models.Experience.GetAdmiralExpForNextLevel(this.RawData.api_level, this.RawData.api_experience);

		/// <summary>
		/// 艦隊司令部レベルを取得します。
		/// </summary>
		public int Level => this.RawData.api_level;

		/// <summary>
		/// 提督のランク名 (元帥, 大将, 中将, ...) を取得します。
		/// </summary>
		public string Rank => Models.Rank.GetName(this.RawData.api_rank);

		/// <summary>
		/// 出撃時の勝利数を取得します。
		/// </summary>
		public int SortieWins => this.RawData.api_st_win;

		/// <summary>
		/// 出撃時の敗北数を取得します。
		/// </summary>
		public int SortieLoses => this.RawData.api_st_lose;

		/// <summary>
		/// 出撃時の勝率を取得します。
		/// </summary>
		public double SortieWinningRate
		{
			get
			{
				var battleCount = this.RawData.api_st_win + this.RawData.api_st_lose;
				if (battleCount == 0) return 0.0;
				return this.RawData.api_st_win / (double)battleCount;
			}
		}

		/// <summary>
		/// 司令部に所属できる艦娘の最大値を取得します。
		/// </summary>
		public int MaxShipCount => this.RawData.api_max_chara;

		/// <summary>
		/// 司令部が保有できる装備アイテムの最大値を取得します。
		/// </summary>
		public int MaxSlotItemCount => this.RawData.api_max_slotitem + 3;


		internal Admiral(kcsapi_basic rawData)
			: base(rawData)
		{
			this.Comment = this.RawData.api_comment;
		}

		public override string ToString()
		{
			return $"ID = {this.MemberId}, Nickname = \"{this.Nickname}\", Level = {this.Level}, Rank = \"{this.Rank}\"";
		}
	}
}
