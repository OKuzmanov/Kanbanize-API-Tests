using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace Kanbanize.Api.Tests
{
    public class CardsAPITests
    {
        private RestClient client;
        private const string subdomain = "gandv";
        private const string apiKey = "OhKzIc9smScLvkY2qoEErS2MtIsiSPaNYb05b84r";
        private string apiUrl = "https://" + subdomain + ".kanbanize.com/index.php/api/kanbanize";

        [SetUp]
        public void Setup()
        {
            this.client = new RestClient();
        }

        [Test]
        public void Test_GetAllProjectsAndBoards()
        {
            RestRequest request = new RestRequest(apiUrl + "/get_projects_and_boards/", Method.Post);
            request.AddHeader("apikey", apiKey);

            RestResponse response = this.client.Execute(request);

            StringReader strReader = new StringReader(response.Content);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(RootXmlProjectsAndBoards));
            RootXmlProjectsAndBoards? rootXmlProjectsAndBoards = (RootXmlProjectsAndBoards)xmlSerializer.Deserialize(strReader);

            Assert.That(HttpStatusCode.OK , Is.EqualTo(response.StatusCode));
            Assert.That(rootXmlProjectsAndBoards.projects.projectItems.Count > 0);
        }

        [Test]
        public void Test_CreateCard_ValidData()
        {
            const string newTitle = "Test Task RestSharp";
            const string newDescription = "Test task description RestSharp";
            const string newPriority = "High";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string column = "In Progress";

            //Get All Projects And Boards
            RootXmlProjectsAndBoards? rootXmlProjectsAndBoards = this.GetAllProjectsAndBoards();

            //Get the board id of the first board in the first project.
            long boardId = rootXmlProjectsAndBoards.projects.projectItems[0].boards.boardItems[0].id;

            int countCardsInColumnBefore = this.GetAllTasksFromBoard(boardId, column).cards.Count;

            //Create new card.
            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = column,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);

            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);
            long createdCardId = rootXmlCreateCards.id;

            //Assert the number of cards in the colum has increase

            int countCardsInColumnAfter = this.GetAllTasksFromBoard(boardId, column).cards.Count;

            Assert.That(countCardsInColumnAfter == countCardsInColumnBefore + 1);

            //Get the newly created card by its id and the board id.
            RootXmlGetTaskDetails rootXmlGetTaskDetails = this.GetTaskDetails(boardId, createdCardId);

            Assert.That(rootXmlGetTaskDetails.title, Is.EqualTo(newTitle));
            Assert.That(rootXmlGetTaskDetails.description, Is.EqualTo(newDescription));
            Assert.That(rootXmlGetTaskDetails.priority, Is.EqualTo(newPriority));
            Assert.That(rootXmlGetTaskDetails.deadLine, Is.EqualTo(newDate));
        }

        [Test]
        public void Test_CreateCard_InvalidData()
        {
            const string errMsg = "The boardid parameter was not set correctly.";

            //Create new card.
            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseCreateCard.StatusCode);

            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlInvalidCardInput));
            RootXmlInvalidCardInput? rootXmlInvalidCardInput = (RootXmlInvalidCardInput)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            string actualResult = rootXmlInvalidCardInput.errMsg;

            Assert.AreEqual(errMsg, actualResult);
        }

        [Test]
        public void Test_MoveCardFromToDoToDone()
        {
            const string newTitle = "Test Task Move";
            const string newDescription = "Test task to be moved via RestSharp";
            const string newPriority = "Low";
            const string newColor = "#f37325";
            const string newDate = "2022-10-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            //Create new card in column To Do.
            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);
            long createdCardId = rootXmlCreateCards.id;

            //Move the newly created task to column Done.
            const string updatedColumn = "Done";
            const string swimLane = "Default lane";

            RestRequest moveRequest = new RestRequest(apiUrl + "/move_task/", Method.Post);
            moveRequest.AddHeader("apikey", apiKey);
            moveRequest.AddBody(new
            {
                boardid = boardId,
                taskid = createdCardId,
                column = updatedColumn,
                lane = swimLane
            });

            RestResponse moveResponse = this.client.Execute(moveRequest);

            Assert.AreEqual(HttpStatusCode.OK, moveResponse.StatusCode);

            //Get all cards from column and assert the newly created card is moved.
            RestRequest getAllFromColumnRequest = new RestRequest(apiUrl + "/get_all_tasks/", Method.Post);
            getAllFromColumnRequest.AddHeader("apikey", apiKey);
            getAllFromColumnRequest.AddBody(new
            {
                boardid = boardId,
                column = updatedColumn,
            });

            RestResponse getAllFromColumnResponse = this.client.Execute(getAllFromColumnRequest);

            Assert.AreEqual(HttpStatusCode.OK, getAllFromColumnResponse.StatusCode);

            StringReader strReaderGetAllFromColumn = new StringReader(getAllFromColumnResponse.Content);

            XmlSerializer xmlSerializerGetAllFromColumn = new XmlSerializer(typeof(RootXmlAllCardsFromBoard));
            RootXmlAllCardsFromBoard? rootXmlAllCardsFromBoard = (RootXmlAllCardsFromBoard)xmlSerializerGetAllFromColumn.Deserialize(strReaderGetAllFromColumn);

            int lastCardIndex = rootXmlAllCardsFromBoard.cards.Count - 1;

            if (lastCardIndex < 0)
            {
                Assert.Fail("Task has not moved to the proper column!");
            }

            Assert.That(rootXmlAllCardsFromBoard.cards[lastCardIndex].taskId, Is.EqualTo(createdCardId));
            Assert.That(rootXmlAllCardsFromBoard.cards[lastCardIndex].title, Is.EqualTo(newTitle));
            Assert.That(rootXmlAllCardsFromBoard.cards[lastCardIndex].description, Is.EqualTo(newDescription));
            Assert.That(rootXmlAllCardsFromBoard.cards[lastCardIndex].priority, Is.EqualTo(newPriority));
            Assert.That(rootXmlAllCardsFromBoard.cards[lastCardIndex].deadLine, Is.EqualTo(newDate));
        }

        [Test]
        public void Test_EditTask_ValidData()
        {
            const string newTitle = "Test Task Edit";
            const string newDescription = "Test task to be edited via RestSharp";
            const string newPriority = "Average";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            //Create new card in column To Do.
            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            long idOfTheCreatedCard = rootXmlCreateCards.id;

            //Edit newly created task
            const string editedTitle = "Successfully Edited Title";
            const string editedDescription = "Successfully Edited Description";
            const string editedPriority = "Critical";
            const string editedColour = "#067db7";

            RestRequest requestEditCard = new RestRequest(apiUrl + "/edit_task/", Method.Post);
            requestEditCard.AddHeader("apikey", apiKey);
            requestEditCard.AddBody(new
            {
                boardid = boardId, 
                taskid = idOfTheCreatedCard, 
                title = editedTitle, 
                description = editedDescription, 
                priority = editedPriority,
                color = editedColour
            });

            RestResponse responseEditCard = this.client.Execute(requestEditCard);

            Assert.AreEqual(HttpStatusCode.OK, responseEditCard.StatusCode);

            //Assert that card details are changed
            RootXmlGetTaskDetails? rootXmlGetTaskDetails = this.GetTaskDetails(boardId, idOfTheCreatedCard);

            Assert.That(editedTitle, Is.EqualTo(rootXmlGetTaskDetails.title));
            Assert.That(editedDescription, Is.EqualTo(rootXmlGetTaskDetails.description));
            Assert.That(editedPriority, Is.EqualTo(rootXmlGetTaskDetails.priority));
            Assert.That(editedColour, Is.EqualTo(rootXmlGetTaskDetails.color));
        }

        [Test]
        public void Test_EditTask_InvalidData()
        {
            const string editedTitle = "Successfully Edited Title";
            const string editedDescription = "Successfully Edited Description";
            const string editedPriority = "Critical";
            const string editedColour = "#067db7";

            RestRequest request = new RestRequest(apiUrl + "/edit_task/", Method.Post);
            request.AddHeader("apikey", apiKey);
            request.AddBody(new
            {
                title = editedTitle,
                description = editedDescription,
                priority = editedPriority,
                color = editedColour
            });

            RestResponse response = this.client.Execute(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            const string expectedErrMsg = "The taskid parameter was not set correctly.";

            StringReader strRead = new StringReader(response.Content);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(RootXmlInvalidCardInput));
            RootXmlInvalidCardInput? rootXmlInvalidCardInput = (RootXmlInvalidCardInput)xmlSerializer.Deserialize(strRead);

            Assert.AreEqual(expectedErrMsg , rootXmlInvalidCardInput.errMsg);
        }

        [Test]
        public void Test_DeleteCard()
        {
            const string newTitle = "Test Task Edit";
            const string newDescription = "Test task to be edited via RestSharp";
            const string newPriority = "Average";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            //Create new card in column To Do.
            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            long idNewlyCreatedCard = rootXmlCreateCards.id;

            int countCardsToDoColumnBefore = this.GetAllTasksFromBoard(boardId, newColumn).cards.Count;

            //Delete newly created card
            RestRequest requestDelete = new RestRequest(apiUrl + "/delete_task/", Method.Post);
            requestDelete.AddHeader("apikey", apiKey);
            requestDelete.AddBody(new
            {
                boardid = boardId,
                taskid = idNewlyCreatedCard
            });

            RestResponse responseDeleteCard = this.client.Execute(requestDelete);

            Assert.AreEqual(HttpStatusCode.OK, responseDeleteCard.StatusCode);

            int countCardsToDoColumnAfter = this.GetAllTasksFromBoard(boardId, newColumn).cards.Count;

            Assert.That(countCardsToDoColumnAfter == countCardsToDoColumnBefore - 1);
        }

        [Test]
        public void Test_AddSubtask_ValidData()
        {
            //Create new card in column To Do.
            const string newTitle = "Test Task Edit";
            const string newDescription = "Test task to be edited via RestSharp";
            const string newPriority = "Average";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            long newlyCreatedCardID = rootXmlCreateCards.id;

            //Add a new Subtask in the newly created Card
            const string subtaskTitle = "Sample subtask title";

            RestRequest requestAddSubtask = new RestRequest(apiUrl + "/add_subtask/", Method.Post);
            requestAddSubtask.AddHeader("apikey", apiKey);
            requestAddSubtask.AddBody(new {
                taskparent = newlyCreatedCardID,
                title = subtaskTitle
            });

            RestResponse responseAddSubtask = this.client.Execute(requestAddSubtask);

            Assert.AreEqual(HttpStatusCode.OK, responseAddSubtask.StatusCode);

            StringReader strReader = new StringReader(responseAddSubtask.Content);

            XmlSerializer? xmlSerializerAddSubtask = new XmlSerializer(typeof(RootXmlAddSubtask));
            RootXmlAddSubtask? rootXmlAddSubtask = (RootXmlAddSubtask)xmlSerializerAddSubtask.Deserialize(strReader);

            long newlyCreatedSubtaskID = rootXmlAddSubtask.id;

            RootXmlGetTaskDetails? rootXmlGetTaskDetails = this.GetTaskDetails(boardId, newlyCreatedCardID);

            List<SubtaskDetail> subtasks = rootXmlGetTaskDetails.subtaskdetails.subtasks;

            bool isSubTaskCreated = false;

            foreach (SubtaskDetail subtask in subtasks)
            {
                if (subtask.id == newlyCreatedSubtaskID)
                {
                    Assert.AreEqual(subtaskTitle, subtask.title);
                    isSubTaskCreated = true;
                }
            }

            if (!isSubTaskCreated)
            {
                Assert.Fail("Subtask is not created properly!");
            }
        }

        [Test]
        public void Test_AddSubtask_InvalidData()
        {
            const string expectedErrMsg = "The taskparent parameter was not set correctly.";
            const string subtaskTitle = "Sample subtask title";

            RestRequest requestAddSubtask = new RestRequest(apiUrl + "/add_subtask/", Method.Post);
            requestAddSubtask.AddHeader("apikey", apiKey);
            requestAddSubtask.AddBody(new
            {
                title = subtaskTitle
            });

            RestResponse responseAddSubtask = this.client.Execute(requestAddSubtask);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseAddSubtask.StatusCode);

            StringReader strReader = new StringReader(responseAddSubtask.Content);

            XmlSerializer? xmlSerializerAddSubtask = new XmlSerializer(typeof(RootXmlInvalidCardInput));
            RootXmlInvalidCardInput? rootXmlInvalidCardInput = (RootXmlInvalidCardInput)xmlSerializerAddSubtask.Deserialize(strReader);

            Assert.AreEqual(expectedErrMsg, rootXmlInvalidCardInput.errMsg);
        }

        [Test]
        public void Test_EdditSubtask_ValidData()
        {
            //Create new card in column To Do.
            const string newTitle = "Test Task Edit";
            const string newDescription = "Test task to be edited via RestSharp";
            const string newPriority = "Average";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            long newlyCreatedCardID = rootXmlCreateCards.id;

            //Add a new Subtask in the newly created Card
            const string subtaskTitle = "Sample subtask title";

            RestRequest requestAddSubtask = new RestRequest(apiUrl + "/add_subtask/", Method.Post);
            requestAddSubtask.AddHeader("apikey", apiKey);
            requestAddSubtask.AddBody(new
            {
                taskparent = newlyCreatedCardID,
                title = subtaskTitle
            });

            RestResponse responseAddSubtask = this.client.Execute(requestAddSubtask);

            Assert.AreEqual(HttpStatusCode.OK, responseAddSubtask.StatusCode);

            StringReader strReader = new StringReader(responseAddSubtask.Content);

            XmlSerializer? xmlSerializerAddSubtask = new XmlSerializer(typeof(RootXmlAddSubtask));
            RootXmlAddSubtask? rootXmlAddSubtask = (RootXmlAddSubtask)xmlSerializerAddSubtask.Deserialize(strReader);

            long newlyCreatedSubtaskID = rootXmlAddSubtask.id;

            //Edit newly created Subtask
            const string editedTitle = "Edited subtask title";
            const string completeStatus = "1";

            RestRequest requestEditSubtask = new RestRequest(apiUrl + "/edit_subtask/", Method.Post);
            requestEditSubtask.AddHeader("apikey", apiKey);
            requestEditSubtask.AddBody(new
            {
                boardid = boardId,
                subtaskid = newlyCreatedSubtaskID,
                title = editedTitle,
                complete = completeStatus
            });

            RestResponse responseEditTask = this.client.Execute(requestEditSubtask);

            Assert.AreEqual(HttpStatusCode.OK, responseEditTask.StatusCode);

            //Assert subtask is edited
            RootXmlGetTaskDetails? rootXmlGetTaskDetails = this.GetTaskDetails(boardId, newlyCreatedCardID);

            List<SubtaskDetail> subtasks = rootXmlGetTaskDetails.subtaskdetails.subtasks;

            foreach (SubtaskDetail subtask in subtasks)
            {
                if (subtask.id == newlyCreatedSubtaskID)
                {
                    Assert.AreEqual(editedTitle, subtask.title);
                }
            }
        }


        [Test]
        public void Test_EdditSubtask_InvalidData()
        {
            const string expectedErrMsg = "The boardid parameter was not set correctly.";
            const string editedTitle = "Edited subtask title";
            const string completeStatus = "1";

            RestRequest requestEditSubtask = new RestRequest(apiUrl + "/edit_subtask/", Method.Post);
            requestEditSubtask.AddHeader("apikey", apiKey);
            requestEditSubtask.AddBody(new
            {
                title = editedTitle,
                complete = completeStatus
            });

            RestResponse responseEditTask = this.client.Execute(requestEditSubtask);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseEditTask.StatusCode);

            StringReader strReader = new StringReader(responseEditTask.Content);

            XmlSerializer xmlSerializerErrMsg = new XmlSerializer(typeof(RootXmlInvalidCardInput));
            RootXmlInvalidCardInput? rootXmlInvalidCardInput = (RootXmlInvalidCardInput)xmlSerializerErrMsg.Deserialize(strReader);

            Assert.AreEqual(expectedErrMsg, rootXmlInvalidCardInput.errMsg);
        }

        [Test]
        public void Test_AddComment_ValidData()
        {
            //Create new card in column To Do.
            const string newTitle = "Test Task Edit";
            const string newDescription = "Test task to be edited via RestSharp";
            const string newPriority = "Average";
            const string newColor = "#f37325";
            const string newDate = "2022-12-01";
            const string newColumn = "To Do";

            //Get the board id of the first board in the first project.
            long boardId = this.GetAllProjectsAndBoards().projects.projectItems[0].boards.boardItems[0].id;

            RestRequest requestCreateCard = new RestRequest(apiUrl + "/create_new_task/", Method.Post);
            requestCreateCard.AddHeader("apikey", apiKey);
            requestCreateCard.AddBody(new
            {
                boardid = boardId,
                title = newTitle,
                description = newDescription,
                priority = newPriority,
                color = newColor,
                deadline = newDate,
                column = newColumn,
                returntaskdetails = "1"
            });

            RestResponse responseCreateCard = this.client.Execute(requestCreateCard);

            Assert.AreEqual(HttpStatusCode.OK, responseCreateCard.StatusCode);
            StringReader strReaderCreateCard = new StringReader(responseCreateCard.Content);

            XmlSerializer xmlSerializerCreateCard = new XmlSerializer(typeof(RootXmlCreateCards));
            RootXmlCreateCards? rootXmlCreateCards = (RootXmlCreateCards)xmlSerializerCreateCard.Deserialize(strReaderCreateCard);

            long newlyCreatedCardID = rootXmlCreateCards.id;

            //Add a comment in the newly created Card
            const string newComment = "This is a new test comment";

            RestRequest requestAddComment = new RestRequest(apiUrl + "/add_comment/", Method.Post);
            requestAddComment.AddHeader("apikey", apiKey);
            requestAddComment.AddBody(new
            {
                taskid = newlyCreatedCardID,
                comment = newComment
            });

            RestResponse responseAddComment = this.client.Execute(requestAddComment);

            Assert.AreEqual(HttpStatusCode.OK, responseAddComment.StatusCode);

            StringReader strReaderAddComment = new StringReader(responseAddComment.Content);

            XmlSerializer xmlSerializerAddComment = new XmlSerializer(typeof(RootXmlAddComment));
            RootXmlAddComment? rootXmlAddComment = (RootXmlAddComment)xmlSerializerAddComment.Deserialize(strReaderAddComment);

            long commentId = rootXmlAddComment.commentId;

            //Get newly created card details and assert the comment is successfully created.
            RootXmlGetTaskDetails? rootXmlGetTaskDetails = this.GetTaskDetails(boardId, newlyCreatedCardID);

            List<CommentDetails> comments = rootXmlGetTaskDetails.commentItems.comments;

            bool isCommentCreated = false;
            foreach (CommentDetails comment in comments)
            {
                if (comment.commentId == commentId)
                {
                    Assert.AreEqual(newComment, comment.text);
                    isCommentCreated = true;
                }
            }

            if (!isCommentCreated)
            {
                Assert.Fail("Comment is not created properly!");
            }
        }

        [Test]
        public void Test_AddComment_InvalidData()
        {
            const string expectedErrMsg = "The taskid parameter was not set correctly.";
            const string newComment = "This is a new test comment";

            RestRequest requestAddComment = new RestRequest(apiUrl + "/add_comment/", Method.Post);
            requestAddComment.AddHeader("apikey", apiKey);

            RestResponse responseAddComment = this.client.Execute(requestAddComment);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseAddComment.StatusCode);

            StringReader strReaderAddComment = new StringReader(responseAddComment.Content);

            XmlSerializer xmlSerializerAddComment = new XmlSerializer(typeof(RootXmlInvalidCardInput));
            RootXmlInvalidCardInput? rootXmlInvalidCardInput = (RootXmlInvalidCardInput)xmlSerializerAddComment.Deserialize(strReaderAddComment);

            Assert.AreEqual(expectedErrMsg, rootXmlInvalidCardInput.errMsg);
        }

            private RootXmlAllCardsFromBoard GetAllTasksFromBoard(long boardId, params string[] columns)
        {
            if (columns.Length == 0)
            {
                RestRequest request = new RestRequest(apiUrl + "/get_all_tasks/", Method.Post);
                request.AddHeader("apikey", apiKey);
                request.AddBody(new
                {
                    boardid = boardId
                });

                RestResponse response = this.client.Execute(request);

                StringReader strReader = new StringReader(response.Content);

                XmlSerializer xmlSerializerGetAllFromColumn = new XmlSerializer(typeof(RootXmlAllCardsFromBoard));
                RootXmlAllCardsFromBoard? rootXmlAllCardsFromBoard = (RootXmlAllCardsFromBoard)xmlSerializerGetAllFromColumn.Deserialize(strReader);

                return rootXmlAllCardsFromBoard;
            } else
            {
                RestRequest request = new RestRequest(apiUrl + "/get_all_tasks/", Method.Post);
                request.AddHeader("apikey", apiKey);
                request.AddBody(new
                {
                    boardid = boardId,
                    column = columns[0]
                });

                RestResponse response = this.client.Execute(request);

                StringReader strReader = new StringReader(response.Content);

                XmlSerializer xmlSerializerGetAllFromColumn = new XmlSerializer(typeof(RootXmlAllCardsFromBoard));
                RootXmlAllCardsFromBoard? rootXmlAllCardsFromBoard = (RootXmlAllCardsFromBoard)xmlSerializerGetAllFromColumn.Deserialize(strReader);

                return rootXmlAllCardsFromBoard;
            }
        }

        private RootXmlGetTaskDetails? GetTaskDetails(long boardId, long createdCardId)
        {
            RestRequest request = new RestRequest(apiUrl + "/get_task_details/", Method.Post);
            request.AddHeader("apikey", apiKey);
            request.AddBody(new {boardid = boardId, taskid = createdCardId, comments = "yes"});

            RestResponse response = this.client.Execute(request);

            StringReader strReader = new StringReader(response.Content);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(RootXmlGetTaskDetails));
            RootXmlGetTaskDetails? rootXmlGetTaskDetails = (RootXmlGetTaskDetails)xmlSerializer.Deserialize(strReader);

            return rootXmlGetTaskDetails;
        }

        private RootXmlProjectsAndBoards? GetAllProjectsAndBoards()
        {
            RestRequest request = new RestRequest(apiUrl + "/get_projects_and_boards/", Method.Post);
            request.AddHeader("apikey", apiKey);

            RestResponse response = this.client.Execute(request);

            StringReader strReader = new StringReader(response.Content);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(RootXmlProjectsAndBoards));
            RootXmlProjectsAndBoards? rootXmlProjectsAndBoards = (RootXmlProjectsAndBoards)xmlSerializer.Deserialize(strReader);

            return rootXmlProjectsAndBoards;
        }
    }
}