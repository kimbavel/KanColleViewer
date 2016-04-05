﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using MetroTrilithon.Threading.Tasks;

namespace Grabacr07.KanColleWrapper
{
	public class KanColleClient : Notifier
	{
		#region singleton

		public static KanColleClient Current { get; } = new KanColleClient();

		#endregion

		public IKanColleClientSettings Settings { get; set; }

		/// <summary>
		/// 艦これの通信をフックするプロキシを取得します。
		/// </summary>
		public KanColleProxy Proxy { get; private set; }

		/// <summary>
		/// ユーザーに依存しないマスター情報を取得します。
		/// </summary>
		public Master Master { get; private set; }

		/// <summary>
		/// 母港の情報を取得します。
		/// </summary>
		public Homeport Homeport { get; private set; }
		
		/// <summary>
		/// 번역
		/// </summary>
		public Translations Translations { get; private set; }

		/// <summary>
		/// 번역파일 및 기타 업데이트
		/// </summary>
		public Updater Updater { get; private set; }

		/// <summary>
		/// 자동번역기
		/// </summary>
		public WebTranslator WebTranslator { get; private set; }

		/// <summary>
		/// 기록
		/// </summary>
		public Logger Logger { get; private set; }

		#region IsStarted 変更通知プロパティ

		private bool _IsStarted;

		/// <summary>
		/// 艦これが開始されているかどうかを示す値を取得します。
		/// </summary>
		public bool IsStarted
		{
			get { return this._IsStarted; }
			set
			{
				if (this._IsStarted != value)
				{
					this._IsStarted = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsInSortie 変更通知プロパティ

		private bool _IsInSortie;

		/// <summary>
		/// 艦隊が出撃中かどうかを示す値を取得します。
		/// </summary>
		public bool IsInSortie
		{
			get { return this._IsInSortie; }
			private set
			{
				if (this._IsInSortie != value)
				{
					this._IsInSortie = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		private KanColleClient()
		{
			this.Initialieze();

			var start = this.Proxy.api_req_map_start;
			var end = this.Proxy.api_port;

			this.Proxy.ApiSessionSource
				.SkipUntil(start.Do(_ => this.IsInSortie = true))
				.TakeUntil(end)
				.Finally(() => this.IsInSortie = false)
				.Repeat()
				.Subscribe();
		}


		public void Initialieze()
		{
			this.Translations = new Translations();
			this.WebTranslator = new WebTranslator();
			this.Updater = new Updater();

			var proxy = this.Proxy ?? (this.Proxy = new KanColleProxy());
            var requireInfo = proxy.api_get_member_require_info.TryParse<kcsapi_require_info>().FirstAsync().ToTask();
			//var basic = proxy.api_get_member_basic.TryParse<kcsapi_basic>().FirstAsync().ToTask();
			//var kdock = proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().FirstAsync().ToTask();
			//var sitem = proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().FirstAsync().ToTask();

			proxy.api_start2.FirstAsync().Subscribe(async session =>
			{
				var timeout = Task.Delay(TimeSpan.FromSeconds(20));
				var canInitialize = await Task.WhenAny(new Task[] { requireInfo }.WhenAll(), timeout) != timeout;

				// タイムアウト仕掛けてるのは、今後のアップデートで basic, kdock, slot_item のいずれかが来なくなったときに
				// 起動できなくなる (IsStarted を true にできなくなる) のを防ぐため
				// -----
				// ま、そんな規模の変更があったらそもそもまともに動作せんだろうがな ☝(◞‸◟)☝ 野良ツールはつらいよ

				SvData<kcsapi_start2> svd;
				if (!SvData.TryParse(session, out svd)) return;

				this.Master = new Master(svd.Data);
				if (this.Homeport == null) this.Homeport = new Homeport(proxy);

				if (canInitialize)
				{
                    var data = await requireInfo;
					this.Homeport.UpdateAdmiral(data.Data.api_basic);
					this.Homeport.Itemyard.Update(data.Data.api_slot_item);
					this.Homeport.Dockyard.Update(data.Data.api_kdock);
				}

				this.IsStarted = true;
			});
			this.Logger = new Logger(proxy);
		}
	}
}
