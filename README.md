# Topee 購物商城
資料庫系統設計 期末專題

## 需求
* 搜尋
  * 關鍵字搜尋
  * 分類搜尋
  * 隨機搜尋
* 使用者
  * 檢視
  * TO幣
  * 修改密碼、註冊、登入、登出
* 商品
  * 檢視
  * 購物車、收藏
  * 結帳 
* 店家
  * 檢視
  * 追蹤
* 其他
  * 防呆機制
  * 對未登入者屏蔽網頁、強迫登入
  
## 資料庫設計
* 主資料表
  * User
  * Store (資料數 : 12)
  * Goods (資料數 : 430)
  * Classify (資料數 : 48)
* 關聯資料表:
  * hashtag ( Goods, Classify)
  * shop_car ( User, Goods)
  * favorite_goods ( User, Goods)
  * favorite_store ( User, Store)
  
  ## 正規化
  * 第一正規化
    * 原本Classify一個欄位會有多項的資料，所以我們把它移出來，使用id去做商品的分類。
  * 第二正規化
    * 滿足沒有部份相依，我們有把User, Store, Goods獨立為一個Table，避免資料重複，透過foreign key來將他們關聯起來；另外，新增三個Table來儲存使用者所使用者所購買的商品、收藏的商品以及追蹤的店家，並將他們跟User做關聯。
  * 第三正規化
    * 滿足沒有遞移相依。
  
  

