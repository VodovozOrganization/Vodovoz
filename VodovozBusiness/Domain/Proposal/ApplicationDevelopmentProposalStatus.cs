using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Proposal
{
    public enum ApplicationDevelopmentProposalStatus
    {
        [Display(Name = "Новое")]
        New,
        [Display(Name = "Отправлено")]
        Sent,
        [Display(Name = "Обрабатывается")]
        Processing,
        [Display(Name = "Отклонено")]
        Rejected,
        [Display(Name = "Формирование задач")]
        CreatingTasks,
        [Display(Name = "Выполнение задач")]
        TasksExecution,
        [Display(Name = "Задачи выполнены")]
        TasksCompleted
    }
}